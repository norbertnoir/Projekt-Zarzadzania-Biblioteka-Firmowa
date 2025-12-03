"use client"

import { useEffect, useState } from "react"
import { Navigation } from "@/components/navigation"
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Book, Users, FileText, TrendingUp, ArrowRight, Clock, Calendar } from "lucide-react"
import api from "@/lib/api"
import { booksApi } from "@/lib/api/books"
import { loansApi } from "@/lib/api/loans"
import { useAuth } from "@/contexts/AuthContext"
import { Book as BookType, Loan as LoanType } from "@/types"
import Link from "next/link"

interface DashboardStats {
    totalBooks: number
    activeLoans: number
    overdueLoans: number
    totalEmployees: number
}

export default function Home() {
    const [stats, setStats] = useState<DashboardStats | null>(null)
    const [recentBooks, setRecentBooks] = useState<BookType[]>([])
    const [myLoans, setMyLoans] = useState<LoanType[]>([])
    const { isAuthenticated, user, isAdmin } = useAuth()

    useEffect(() => {
        const fetchData = async () => {
            try {
                // Pobranie statystyk dashboardu (liczba książek, wypożyczeń, pracowników)
                const statsResponse = await api.get("/api/Reports/dashboard")
                setStats(statsResponse.data)

                // Pobranie ostatnio dodanych książek (symulacja przez pobranie wszystkich i sortowanie)
                const allBooks = await booksApi.getAll()
                const sortedBooks = allBooks.sort((a, b) => b.id - a.id).slice(0, 4)
                setRecentBooks(sortedBooks)

                // Pobranie aktywnych wypożyczeń zalogowanego użytkownika
                if (isAuthenticated && user?.employeeId) {
                    const loans = await loansApi.getByEmployee(user.employeeId)
                    const active = loans.filter(l => !l.returnDate).slice(0, 3)
                    setMyLoans(active)
                }
            } catch (error) {
                console.error("Failed to fetch home page data", error)
            }
        }

        if (isAuthenticated) {
            // Jeśli zalogowany, pobierz wszystko (w tym wypożyczenia)
            fetchData()
        } else {
            // Jeśli niezalogowany, pobierz tylko dane publiczne
            fetchData()
        }
    }, [isAuthenticated, user])

    return (
        <div className="min-h-screen bg-background">
            <Navigation />
            <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">

                {/* Hero Section */}
                <div className="mb-12 text-center">
                    <h1 className="text-4xl font-bold tracking-tight text-foreground sm:text-6xl mb-4">
                        System Zarządzania Biblioteką Firmy
                    </h1>
                    <p className="mt-4 text-lg text-muted-foreground max-w-2xl mx-auto">
                        Twoje centrum wiedzy. Przeglądaj, wypożyczaj i zarządzaj zasobami bibliotecznymi w prosty i efektywny sposób.
                    </p>
                    <div className="mt-8 flex justify-center gap-4">
                        <Link href="/books">
                            <Button size="lg" className="gap-2">
                                <Book className="h-5 w-5" />
                                Przeglądaj Książki
                            </Button>
                        </Link>
                        {isAuthenticated ? (
                            <Link href="/loans">
                                <Button variant="outline" size="lg" className="gap-2">
                                    <FileText className="h-5 w-5" />
                                    Moje Wypożyczenia
                                </Button>
                            </Link>
                        ) : (
                            <Link href="/login">
                                <Button variant="default" size="lg" className="gap-2">
                                    <Users className="h-5 w-5" />
                                    Zaloguj się
                                </Button>
                            </Link>
                        )}
                    </div>
                </div>

                {/* Stats Grid */}
                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-12">
                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-sm font-medium">Książki</CardTitle>
                            <Book className="h-4 w-4 text-muted-foreground" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{stats?.totalBooks || 0}</div>
                            <p className="text-xs text-muted-foreground">Wszystkie książki w bibliotece</p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-sm font-medium">Aktywne wypożyczenia</CardTitle>
                            <FileText className="h-4 w-4 text-muted-foreground" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{stats?.activeLoans || 0}</div>
                            <p className="text-xs text-muted-foreground">Obecnie wypożyczone</p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-sm font-medium">Pracownicy</CardTitle>
                            <Users className="h-4 w-4 text-muted-foreground" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{stats?.totalEmployees || 0}</div>
                            <p className="text-xs text-muted-foreground">Zarejestrowani pracownicy</p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-sm font-medium">Przeterminowane</CardTitle>
                            <TrendingUp className="h-4 w-4 text-destructive" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold text-destructive">{stats?.overdueLoans || 0}</div>
                            <p className="text-xs text-muted-foreground">Wymagają interwencji</p>
                        </CardContent>
                    </Card>
                </div>

                {/* Content Sections */}
                <div className="grid gap-8 md:grid-cols-2">

                    {/* Recently Added Books */}
                    <div className="space-y-6">
                        <div className="flex items-center justify-between">
                            <h2 className="text-2xl font-bold tracking-tight">Ostatnio dodane</h2>
                            <Link href="/books" className="text-sm text-primary hover:underline flex items-center gap-1">
                                Zobacz wszystkie <ArrowRight className="h-4 w-4" />
                            </Link>
                        </div>
                        <div className="grid gap-4">
                            {recentBooks.map((book) => (
                                <Card key={book.id} className="flex flex-row items-center p-4 gap-4 hover:bg-muted/50 transition-colors">
                                    <div className="h-16 w-12 bg-muted rounded flex items-center justify-center shrink-0">
                                        <Book className="h-8 w-8 text-muted-foreground/50" />
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <h3 className="font-semibold truncate">{book.title}</h3>
                                        <p className="text-sm text-muted-foreground truncate">{book.authors?.map(a => a.fullName).join(", ")}</p>
                                        <div className="flex items-center gap-2 mt-1">
                                            <span className="inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80">
                                                {book.categoryName}
                                            </span>
                                            <span className="text-xs text-muted-foreground">{book.year}</span>
                                        </div>
                                    </div>
                                </Card>
                            ))}
                            {recentBooks.length === 0 && (
                                <p className="text-muted-foreground text-sm">Brak książek do wyświetlenia.</p>
                            )}
                        </div>
                    </div>

                    {/* My Active Loans (or Info for Guests) */}
                    <div className="space-y-6">
                        <div className="flex items-center justify-between">
                            <h2 className="text-2xl font-bold tracking-tight">
                                {isAuthenticated ? "Twoje aktywne wypożyczenia" : "Dlaczego warto?"}
                            </h2>
                            {isAuthenticated && (
                                <Link href="/loans" className="text-sm text-primary hover:underline flex items-center gap-1">
                                    Pełna historia <ArrowRight className="h-4 w-4" />
                                </Link>
                            )}
                        </div>

                        {isAuthenticated ? (
                            <div className="grid gap-4">
                                {myLoans.map((loan) => (
                                    <Card key={loan.id} className="p-4 hover:bg-muted/50 transition-colors">
                                        <div className="flex items-start justify-between">
                                            <div>
                                                <h3 className="font-semibold">{loan.bookTitle}</h3>
                                                <div className="flex items-center gap-2 mt-2 text-sm text-muted-foreground">
                                                    <Calendar className="h-4 w-4" />
                                                    <span>Wypożyczono: {new Date(loan.loanDate).toLocaleDateString()}</span>
                                                </div>
                                            </div>
                                            <div className="flex flex-col items-end gap-1">
                                                <div className={`flex items-center gap-1 text-xs font-medium px-2 py-1 rounded-full ${new Date(loan.dueDate) < new Date()
                                                    ? "bg-destructive/10 text-destructive"
                                                    : "bg-green-500/10 text-green-600"
                                                    }`}>
                                                    <Clock className="h-3 w-3" />
                                                    {new Date(loan.dueDate) < new Date() ? "Przeterminowane" : "W terminie"}
                                                </div>
                                                <span className="text-xs text-muted-foreground">
                                                    Do zwrotu: {new Date(loan.dueDate).toLocaleDateString()}
                                                </span>
                                            </div>
                                        </div>
                                    </Card>
                                ))}
                                {myLoans.length === 0 && (
                                    <Card className="p-6 text-center text-muted-foreground">
                                        <p>Nie masz obecnie żadnych aktywnych wypożyczeń.</p>
                                        <Link href="/books">
                                            <Button variant="link" className="mt-2">Wypożycz coś nowego</Button>
                                        </Link>
                                    </Card>
                                )}
                            </div>
                        ) : (
                            <Card className="p-6">
                                <ul className="space-y-4">
                                    <li className="flex items-start gap-3">
                                        <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                                            <Book className="h-4 w-4 text-primary" />
                                        </div>
                                        <div>
                                            <h3 className="font-semibold">Bogaty księgozbiór</h3>
                                            <p className="text-sm text-muted-foreground">Dostęp do setek książek branżowych i beletrystyki.</p>
                                        </div>
                                    </li>
                                    <li className="flex items-start gap-3">
                                        <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                                            <Clock className="h-4 w-4 text-primary" />
                                        </div>
                                        <div>
                                            <h3 className="font-semibold">Wygodne wypożyczenia</h3>
                                            <p className="text-sm text-muted-foreground">Rezerwuj i wypożyczaj online bez zbędnych formalności.</p>
                                        </div>
                                    </li>
                                    <li className="flex items-start gap-3">
                                        <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0">
                                            <Users className="h-4 w-4 text-primary" />
                                        </div>
                                        <div>
                                            <h3 className="font-semibold">Społeczność</h3>
                                            <p className="text-sm text-muted-foreground">Dziel się opiniami i polecaj książki współpracownikom.</p>
                                        </div>
                                    </li>
                                </ul>
                            </Card>
                        )}
                    </div>
                </div>
            </main>
        </div>
    )
}
