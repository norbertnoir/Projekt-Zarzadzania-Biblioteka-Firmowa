// Author types
export interface Author {
  id: number
  firstName?: string
  lastName?: string
  fullName?: string
  biography?: string
}

export interface CreateAuthorDto {
  firstName: string
  lastName: string
  biography?: string
}

// Category types
export interface Category {
  id: number
  name?: string
  description?: string
}

// Book types
export interface Book {
  id: number
  title?: string
  isbn?: string
  publisher?: string
  year: number
  pages: number
  description?: string
  isAvailable: boolean
  categoryId: number
  categoryName?: string
  authors?: Author[]
}

export interface CreateBookDto {
  title?: string
  isbn?: string
  publisher?: string
  year: number
  pages: number
  description?: string
  categoryId: number
  authorIds?: number[]
}

export interface UpdateBookDto extends Partial<CreateBookDto> { }

// Loan types
export interface Loan {
  id: number
  loanDate: string
  returnDate?: string
  dueDate: string
  isReturned: boolean
  notes?: string
  bookId: number
  bookTitle?: string
  employeeId: number
  employeeName?: string
}

export interface CreateLoanDto {
  bookId: number
  employeeId: number
  dueDate: string
  notes?: string
}

export interface UpdateLoanDto {
  bookId?: number
  employeeId?: number
  dueDate?: string
  notes?: string
}

export interface ReturnLoanDto {
  returnDate: string
  notes?: string
}

// Employee types (używane zamiast User)
export interface Employee {
  id: number
  firstName?: string
  lastName?: string
  fullName?: string
  email?: string
  department?: string
  position?: string
  createdAt: string
}

export interface CreateEmployeeDto {
  firstName?: string
  lastName?: string
  email?: string
  department?: string
  position?: string
}

export interface UpdateEmployeeDto extends Partial<CreateEmployeeDto> { }

// User types (dla autoryzacji - z /api/Auth/users)
export interface User {
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

// API Response types
export interface ApiResponse<T> {
  data: T
  message?: string
  success: boolean
}

// Backend zwraca tablice, nie paginację
export type ArrayResponse<T> = T[]
