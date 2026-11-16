"""Run only against the dedicated chapter 11 test API/database. See README.md."""
import base64
import concurrent.futures
import datetime as dt
import hashlib
import hmac
import json
import os
import subprocess
import time
import urllib.error
import urllib.request
import uuid

BASE = os.environ.get("PCLUB_TEST_URL", "http://127.0.0.1:55150/api/v1")
CONTAINER = os.environ.get("PCLUB_TEST_CONTAINER", "pclub-ch11-tests")
DATABASE = "pclub_http_tests"
checks = 0

def sql(statement):
    return subprocess.check_output(["docker", "exec", CONTAINER, "psql", "-U", "pclub", "-d", DATABASE,
        "-v", "ON_ERROR_STOP=1", "-Atc", statement], text=True, encoding="utf-8").strip()

def request(method, path, body=None, token=None, expected=200):
    global checks
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(BASE + path, data=json.dumps(body).encode() if body is not None else None,
        headers=headers, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    raw = response.read().decode()
    assert response.status == expected, f"{method} {path}: expected {expected}, got {response.status}"
    checks += 1
    return json.loads(raw) if raw else None

def login(email, password="Password123!"):
    return request("POST", "/auth/login", {"email": email, "password": password})

def register():
    email = f"{uuid.uuid4()}@example.com"
    body = {"email": email, "password": "Test1234!", "displayName": "HTTP test", "role": "Admin"}
    result = request("POST", "/auth/register", body)
    assert result["role"] == "User" and "passwordHash" not in result
    request("POST", "/auth/register", body, expected=409)
    return result

request("GET", "/clubs")
for path in ["/auth/me", "/wallet", "/wallet/statement"]:
    request("GET", path, expected=401)
request("POST", "/bookings", {}, expected=401)
for password in [None, "", "abc", "password"]:
    request("POST", "/auth/register", {"email": "test@example.com", "password": password, "displayName": "Test"}, expected=400)
request("POST", "/auth/login", {"email": None, "password": None}, expected=400)
request("POST", "/auth/refresh", {"refreshToken": None}, expected=400)
first = register()
second = register()
token = first["accessToken"]
assert request("GET", "/auth/me", token=token)["userId"] == first["userId"]
bad = request("POST", "/auth/login", {"email": first["email"], "password": "Wrong1234"}, expected=401)
absent = request("POST", "/auth/login", {"email": f"{uuid.uuid4()}@example.com", "password": "Wrong1234"}, expected=401)
assert bad["detail"] == absent["detail"]
login(first["email"].upper(), "Test1234!")
request("GET", "/auth/me?access_token=" + token, expected=401)
request("GET", "/auth/me", token=token[:-12] + "bad-signature", expected=401)

# Explicit JWT validation probes: the signing key belongs only to the test API.
key = os.environ["PCLUB_TEST_JWT"]
def encode(value):
    return base64.urlsafe_b64encode(json.dumps(value, separators=(",", ":")).encode()).rstrip(b"=").decode()
payload = json.loads(base64.urlsafe_b64decode(token.split(".")[1] + "=="))
for change in [{"exp": int(time.time()) - 1}, {"iss": "wrong"}, {"aud": "wrong"}]:
    part = encode({**payload, **change})
    unsigned = token.split(".")[0] + "." + part
    signature = base64.urlsafe_b64encode(hmac.new(key.encode(), unsigned.encode(), hashlib.sha256).digest()).rstrip(b"=").decode()
    request("GET", "/auth/me", token=unsigned + "." + signature, expected=401)

owner = login("owner@example.com")
admin = login("admin@example.com")
other_owner = register()
sql(f"UPDATE users SET role='Owner' WHERE id='{other_owner['userId']}'")
other_owner = login(other_owner["email"], "Test1234!")
# Fixture is confined to the disposable database, creates no production data.
club, zone, seat = [str(uuid.uuid4()) for _ in range(3)]
sql(f"""INSERT INTO clubs (id,name,city,address,opening_time,closing_time,status,created_at,description,owner_id)
VALUES ('{club}','HTTP fixture','Test','Test','00:00','00:00','Published',now(),'Test','{owner['userId']}');
INSERT INTO zones (id,club_id,name,price_per_hour_cents,specs) VALUES ('{zone}','{club}','Test',2001,'');
INSERT INTO seats (id,zone_id,label,status,specs) VALUES ('{seat}','{zone}','Test','Active','');""")
request("GET", f"/clubs/{club}")
for identity, status in [(None,401),(token,403),(other_owner["accessToken"],403),(owner["accessToken"],200),(admin["accessToken"],200)]:
    request("PUT", f"/clubs/{club}/status", {"status":"Published"}, identity, status)
    request("PUT", f"/zones/{zone}/price", {"pricePerHourCents":2001}, identity, status)
start = (dt.datetime.now(dt.timezone.utc) + dt.timedelta(days=2)).replace(hour=12, minute=0, second=0, microsecond=0)
booking_body = {"seatId":seat,"startTime":start.isoformat(),"endTime":(start+dt.timedelta(hours=1)).isoformat(),"userId":second["userId"]}
request("POST", "/bookings", booking_body, token, 422)
request("POST", "/wallet/top-up", {"amountCents":100000, "userId":second["userId"]}, token)
assert request("GET", "/wallet", token=second["accessToken"])["balanceCents"] == 0
for amount in [500,10000001]:
    request("POST", "/wallet/top-up", {"amountCents":amount}, token, 400)
booking = request("POST", "/bookings", booking_body, token, 201)
assert booking["userId"] == first["userId"]
request("GET", f"/bookings/{booking['id']}", token=second["accessToken"], expected=403)
request("POST", f"/bookings/{booking['id']}/cancel", token=second["accessToken"], expected=403)
cancelled = request("POST", f"/bookings/{booking['id']}/cancel", token=token)
assert cancelled["refundedCents"] == 2001
assert request("GET", "/wallet", token=token)["isConsistent"]
assert request("GET", "/wallet/statement", token=token)["totalCount"] == 3

rotated = request("POST", "/auth/refresh", {"refreshToken":first["refreshToken"]})
request("POST", "/auth/refresh", {"refreshToken":first["refreshToken"]}, expected=401)
request("POST", "/auth/refresh", {"refreshToken":rotated["refreshToken"]}, expected=401)
session = login(first["email"], "Test1234!")
for _ in range(2):
    request("POST", "/auth/logout", {"refreshToken":session["refreshToken"]}, expected=204)
request("POST", "/auth/refresh", {"refreshToken":session["refreshToken"]}, expected=401)
assert sql("SELECT count(*) FROM users u WHERE balance_cents <> (SELECT coalesce(sum(amount_cents),0) FROM wallet_transactions w WHERE w.user_id=u.id)") == "0"
swagger = json.load(urllib.request.urlopen(BASE.removesuffix("/api/v1") + "/swagger/v1/swagger.json"))
assert swagger["components"]["securitySchemes"]["Bearer"]["scheme"] == "bearer"
assert "userId" not in swagger["components"]["schemas"]["CreateBookingRequest"]["properties"]
assert "userId" not in swagger["components"]["schemas"]["TopUpRequest"]["properties"]
print(f"Passed {checks} HTTP checks; Swagger, identity isolation, roles and ledger verified.")
