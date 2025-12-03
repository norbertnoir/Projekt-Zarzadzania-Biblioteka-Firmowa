"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { Book, Users, FileText, Home, LogOut, Activity } from "lucide-react"
import { cn } from "@/lib/utils"
import { ThemeToggle } from "@/components/theme-toggle"
import { useAuth } from "@/contexts/AuthContext"
import { Button } from "@/components/ui/button"
import { UserAvatar } from "@/components/user-avatar"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

const navigation = [
  { name: "Strona główna", href: "/", icon: Home, adminOnly: false },
  { name: "Dashboard", href: "/dashboard", icon: FileText, adminOnly: false },
  { name: "Książki", href: "/books", icon: Book, adminOnly: false },
  { name: "Wypożyczenia", href: "/loans", icon: FileText, adminOnly: true },
  { name: "Pracownicy", href: "/users", icon: Users, adminOnly: true },
  { name: "Raporty i Logi", href: "/admin/reports", icon: Activity, adminOnly: true },
]

export function Navigation() {
  const pathname = usePathname()
  const router = useRouter()
  const { isAdmin, isAuthenticated, user, logout, loading } = useAuth()

  const handleLogout = () => {
    logout()
    router.push('/login')
  }

  const visibleNavigation = navigation.filter(
    (item) => !item.adminOnly || isAdmin
  )

  // Nie renderuj nawigacji podczas ładowania, aby uniknąć błędów
  if (loading) {
    return (
      <nav className="border-b bg-background">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-foreground">
                Biblioteka Firmy
              </h1>
            </div>
          </div>
        </div>
      </nav>
    )
  }

  return (
    <nav className="border-b bg-background">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
          <div className="flex items-center">
            <h1 className="text-xl font-bold text-foreground">
              Biblioteka Firmy
            </h1>
          </div>
          <div className="flex items-center space-x-4">
            {visibleNavigation.map((item) => {
              const Icon = item.icon
              const isActive = pathname === item.href
              return (
                <Link
                  key={item.name}
                  href={item.href}
                  className={cn(
                    "flex items-center space-x-2 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : "text-foreground/70 hover:bg-accent hover:text-foreground"
                  )}
                >
                  <Icon className="h-4 w-4" />
                  <span>{item.name}</span>
                </Link>
              )
            })}
            <ThemeToggle />
            {isAuthenticated && user && user.id && (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" className="h-9 w-9 p-0">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground text-sm font-semibold">
                      {user?.username?.charAt(0).toUpperCase() || user?.email?.charAt(0).toUpperCase() || "U"}
                    </div>
                    <span className="sr-only">Menu użytkownika</span>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <div className="px-2 py-1.5">
                    <p className="text-sm font-medium">{user?.username || user?.email}</p>
                    {user?.role && (
                      <p className="text-xs text-muted-foreground">{user.role}</p>
                    )}
                  </div>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut className="mr-2 h-4 w-4" />
                    <span>Wyloguj się</span>
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>
        </div>
      </div>
    </nav>
  )
}

