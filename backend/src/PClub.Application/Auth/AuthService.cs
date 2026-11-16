using PClub.Application.Abstractions;
using PClub.Domain.Entities;
using PClub.Domain.Exceptions;

namespace PClub.Application.Auth;

/// <summary>Регистрация, вход и обновление токенов.</summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    /// <summary>Создаёт сервис.</summary>
    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        ITokenService tokens,
        IUnitOfWork unitOfWork,
        IClock clock, ICurrentUser currentUser)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("Пользователь с такой почтой уже есть.");
        }

        var user = new User(
            email: request.Email,
            displayName: request.DisplayName,
            passwordHash: _passwordHasher.Hash(request.Password));

        _users.Add(user);

        return await IssueTokensAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Неверная почта или пароль.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> RefreshAsync(
        RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(
            token => RefreshCoreAsync(request, token), cancellationToken);
        return result ?? throw new UnauthorizedException("Токен обновления недействителен. Войдите снова.");
    }

    private async Task<AuthResultDto?> RefreshCoreAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken)
            ?? throw new UnauthorizedException("Токен обновления недействителен.");

        var now = _clock.UtcNow;
        if (stored.UsedAt is not null)
        {
            await _refreshTokens.RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return null;
        }

        if (!stored.IsActive(now))
        {
            throw new UnauthorizedException("Токен обновления недействителен.");
        }

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден.");

        stored.MarkUsed(now);
        return await IssueTokensAsync(user, cancellationToken, stored.FamilyId);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await LogoutCoreAsync(refreshToken, token);
            return true;
        }, cancellationToken);

    private async Task LogoutCoreAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = _tokens.HashRefreshToken(refreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null)
        {
            return;
        }

        await _refreshTokens.RevokeFamilyAsync(
            stored.FamilyId, _clock.UtcNow, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CurrentUserDto> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.RequireUserId();
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден.");

        return new CurrentUserDto(
            user.Id, user.Email, user.DisplayName, user.Role, user.BalanceCents);
    }

    /// <summary>Выпускает пару токенов и сохраняет refresh в базу.</summary>
    private async Task<AuthResultDto> IssueTokensAsync(
        User user, CancellationToken cancellationToken, Guid? familyId = null)
    {
        var (accessToken, expiresAt) = _tokens.CreateAccessToken(user);
        var (refreshValue, refreshHash) = _tokens.CreateRefreshToken();

        _refreshTokens.Add(new RefreshToken(
            userId: user.Id,
            tokenHash: refreshHash,
            familyId: familyId ?? Guid.NewGuid(),
            now: _clock.UtcNow));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            accessToken,
            refreshValue,
            expiresAt,
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role);
    }
}
