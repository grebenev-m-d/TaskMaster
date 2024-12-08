import { JwtHelperService } from '@auth0/angular-jwt';

// Функция для получения идентификатора пользователя из JWT-токена
export function getUserIdFromJwt(jwtToken: string): string {
  // Декодирование JWT-токена
  const decodedToken = new JwtHelperService().decodeToken(jwtToken);
  // Возврат идентификатора пользователя из токена
  return decodedToken.sub; // Предполагается, что идентификатор пользователя хранится в подтверждении sub
}

// Функция для получения имени пользователя из JWT-токена
export function getUserNameFromJwt(jwtToken: string): string {
  // Декодирование JWT-токена
  const decodedToken = new JwtHelperService().decodeToken(jwtToken);
  // Возврат имени пользователя из токена
  return decodedToken.name; // Предполагается, что имя пользователя хранится в подтверждении name
}

// Функция для получения электронной почты пользователя из JWT-токена
export function getEmailFromJwt(jwtToken: string): string {
  // Декодирование JWT-токена
  const decodedToken = new JwtHelperService().decodeToken(jwtToken);
  // Возврат адреса электронной почты пользователя из токена
  return decodedToken.email; // Предполагается, что электронная почта хранится в подтверждении email
}

// Функция для получения роли пользователя из JWT-токена
export function getRoleFromJwt(jwtToken: string): string {
  // Декодирование JWT-токена
  const decodedToken = new JwtHelperService().decodeToken(jwtToken);
  // Возврат роли пользователя из токена
  return decodedToken.role; // Предполагается, что роль пользователя хранится в подтверждении role
}
