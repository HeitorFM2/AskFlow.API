namespace AskFlow.Application.Common
{
    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string InternalServerError = "INTERNAL_SERVER_ERROR";
        public const string UserNotAuthenticated = "USER_NOT_AUTHENTICATED";

        public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
        public const string AuthRefreshTokenInvalid = "AUTH_REFRESH_TOKEN_INVALID";
        public const string AuthRefreshTokenExpiredOrRevoked = "AUTH_REFRESH_TOKEN_EXPIRED_OR_REVOKED";
        public const string AuthRefreshTokenReuseDetected = "AUTH_REFRESH_TOKEN_REUSE_DETECTED";
        public const string AuthIdentityFailure = "AUTH_IDENTITY_FAILURE";

        public const string PostNotFound = "POST_NOT_FOUND";
        public const string PostNoPermissionToDelete = "POST_NO_PERMISSION_TO_DELETE";

        public const string CommentNotFound = "COMMENT_NOT_FOUND";
        public const string CommentParentNotFound = "COMMENT_PARENT_NOT_FOUND";
        public const string CommentNoPermissionToDelete = "COMMENT_NO_PERMISSION_TO_DELETE";

        public const string EmailRequired = "EMAIL_REQUIRED";
        public const string EmailInvalid = "EMAIL_INVALID";
        public const string PasswordRequired = "PASSWORD_REQUIRED";
        public const string PasswordMinLength = "PASSWORD_MIN_LENGTH";
        public const string PasswordRequiresUppercase = "PASSWORD_REQUIRES_UPPERCASE";
        public const string PasswordRequiresLowercase = "PASSWORD_REQUIRES_LOWERCASE";
        public const string PasswordRequiresDigit = "PASSWORD_REQUIRES_DIGIT";
        public const string IdentificationRequired = "IDENTIFICATION_REQUIRED";
        public const string IdentificationMaxLength = "IDENTIFICATION_MAX_LENGTH";
        public const string RefreshTokenRequired = "REFRESH_TOKEN_REQUIRED";

        public const string PostContentRequired = "POST_CONTENT_REQUIRED";
        public const string PostContentMaxLength = "POST_CONTENT_MAX_LENGTH";

        public const string CommentPostIdInvalid = "COMMENT_POST_ID_INVALID";
        public const string CommentContentRequired = "COMMENT_CONTENT_REQUIRED";
        public const string CommentContentMaxLength = "COMMENT_CONTENT_MAX_LENGTH";

        public const string UserNotFound = "USER_NOT_FOUND";
        public const string AvatarRequired = "AVATAR_REQUIRED";
        public const string AvatarContentTypeInvalid = "AVATAR_CONTENT_TYPE_INVALID";
        public const string AvatarTooLarge = "AVATAR_TOO_LARGE";
        public const string AvatarInvalidContent = "AVATAR_INVALID_CONTENT";
        public const string AvatarUploadFailed = "AVATAR_UPLOAD_FAILED";
    }
}
