"use client"

import { useState, useEffect } from "react"
import { useParams, useRouter } from "next/navigation"
import { Navigation } from "@/components/navigation"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { booksApi } from "@/lib/api/books"
import { loansApi } from "@/lib/api/loans"
import { Book, Loan } from "@/types"
import { ArrowLeft, Calendar, User, BookOpen } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { format } from "date-fns"
import { pl } from "date-fns/locale/pl"

export default function BookDetailsPage() {
  const params = useParams()
  const router = useRouter()
  const { toast } = useToast()
  const [book, setBook] = useState<Book | null>(null)
  const [loans, setLoans] = useState<Loan[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (params.id) {
      loadBook()
      loadLoans()
    }
  }, [params.id])

  const loadBook = async () => {
    try {
      setLoading(true)
      const data = await booksApi.getById(Number(params.id))
      setBook(data)
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Nie udało się załadować książki",
        variant: "destructive",
      })
      router.push("/books")
    } finally {
      setLoading(false)
    }
  }

  const loadLoans = async () => {
    try {
      // Pobierz wszystkie wypożyczenia i filtruj po bookId
      const allLoans = await loansApi.getAll()
      const bookLoans = allLoans.filter(loan => loan.bookId === Number(params.id))
      setLoans(bookLoans)
    } catch (error) {
      // Silently fail - loans might not be available
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-background">
        <Navigation />
        <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="text-center">Ładowanie...</div>
        </main>
      </div>
    )
  }

  if (!book) {
    return null
  }

  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <Button
          variant="ghost"
          onClick={() => router.back()}
          className="mb-4"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Wróć
        </Button>

        <div className="grid gap-6 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-2xl">{book.title}</CardTitle>
              <CardDescription>Szczegóły książki</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {book.authors && book.authors.length > 0 && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Autorzy</p>
                  <p className="text-lg">
                    {book.authors.map(a => a.fullName || `${a.firstName} ${a.lastName}`).join(", ")}
                  </p>
                </div>
              )}
              {book.isbn && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">ISBN</p>
                  <p className="text-lg">{book.isbn}</p>
                </div>
              )}
              <div>
                <p className="text-sm font-medium text-muted-foreground">Rok wydania</p>
                <p className="text-lg">{book.year}</p>
              </div>
              {book.pages && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Liczba stron</p>
                  <p className="text-lg">{book.pages}</p>
                </div>
              )}
              {book.publisher && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Wydawca</p>
                  <p className="text-lg">{book.publisher}</p>
                </div>
              )}
              {book.categoryName && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Kategoria</p>
                  <p className="text-lg">{book.categoryName}</p>
                </div>
              )}
              <div>
                <p className="text-sm font-medium text-muted-foreground">Dostępność</p>
                <span
                  className={`inline-flex rounded-full px-3 py-1 text-sm font-semibold ${
                    book.isAvailable
                      ? "bg-green-500/10 text-green-700 dark:text-green-400 high-contrast:bg-transparent high-contrast:text-foreground"
                      : "bg-red-500/10 text-red-700 dark:text-red-400 high-contrast:bg-transparent high-contrast:text-foreground"
                  }`}
                >
                  {book.isAvailable ? "Dostępna" : "Wypożyczona"}
                </span>
              </div>
              {book.description && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Opis</p>
                  <p className="text-lg">{book.description}</p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Historia wypożyczeń</CardTitle>
              <CardDescription>Lista wszystkich wypożyczeń tej książki</CardDescription>
            </CardHeader>
            <CardContent>
              {loans.length === 0 ? (
                <p className="text-sm text-muted-foreground">Brak historii wypożyczeń</p>
              ) : (
                <div className="space-y-4">
                  {loans.map((loan) => (
                    <div
                      key={loan.id}
                      className="rounded-lg border p-4"
                    >
                      <div className="flex items-center space-x-2 text-sm">
                        <User className="h-4 w-4 text-muted-foreground" />
                        <span>
                          {loan.employeeName || `Pracownik #${loan.employeeId}`}
                        </span>
                      </div>
                      <div className="mt-2 flex items-center space-x-4 text-xs text-muted-foreground">
                        <div className="flex items-center space-x-1">
                          <Calendar className="h-3 w-3" />
                          <span>
                            Wypożyczono:{" "}
                            {format(new Date(loan.loanDate), "dd MMM yyyy", {
                              locale: pl,
                            })}
                          </span>
                        </div>
                        {loan.returnDate && (
                          <div className="flex items-center space-x-1">
                            <BookOpen className="h-3 w-3" />
                            <span>
                              Zwrócono:{" "}
                              {format(new Date(loan.returnDate), "dd MMM yyyy", {
                                locale: pl,
                              })}
                            </span>
                          </div>
                        )}
                      </div>
                      <div className="mt-2">
                        <span
                          className={`inline-flex rounded-full px-2 py-1 text-xs font-semibold ${
                            loan.isReturned
                              ? "bg-muted text-muted-foreground high-contrast:bg-transparent high-contrast:text-foreground"
                              : new Date(loan.dueDate) < new Date() && !loan.isReturned
                              ? "bg-red-500/10 text-red-700 dark:text-red-400 high-contrast:bg-transparent high-contrast:text-foreground"
                              : "bg-blue-500/10 text-blue-700 dark:text-blue-400 high-contrast:bg-transparent high-contrast:text-foreground"
                          }`}
                        >
                          {loan.isReturned
                            ? "Zwrócone"
                            : new Date(loan.dueDate) < new Date()
                            ? "Przeterminowane"
                            : "Aktywne"}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  )
}

