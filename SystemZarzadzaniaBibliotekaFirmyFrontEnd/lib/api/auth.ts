import apiClient from '../api'

export interface LoginDto {
  username?: string
  password?: string
}

export interface RegisterDto {
  username?: string
  email?: string
  password?: string
  role?: string
  employeeId?: number | null
}

export interface AuthResponseDto {
  token?: string
  username?: string
  email?: string
  role?: string
  expiresAt: string
}

export interface UserDto {
  id: number
  username?: string
  email?: string
  role?: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
  employeeId?: number
  employeeName?: string
}

export const authApi = {
  login: async (data: LoginDto): Promise<AuthResponseDto> => {
    const response = await apiClient.post<AuthResponseDto>('/api/Auth/login', data)
    return response.data
  },

  register: async (data: RegisterDto): Promise<AuthResponseDto> => {
    const response = await apiClient.post<AuthResponseDto>('/api/Auth/register', data)
    return response.data
  },

  me: async (): Promise<UserDto> => {
    const response = await apiClient.get<UserDto>('/api/Auth/me')
    return response.data
  },
}

