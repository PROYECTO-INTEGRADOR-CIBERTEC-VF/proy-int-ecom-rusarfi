import { ApiResponse } from '../../core/models/api-response';

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  ConfirmPassword: string;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
}

export type RegisterResponse = ApiResponse<UserDto>;
