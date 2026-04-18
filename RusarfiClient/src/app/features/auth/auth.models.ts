export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]> | null;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
}

export type RegisterResponse = ApiResponse<UserDto>;
