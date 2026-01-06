
"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { authorsApi } from "@/lib/api/authors"
import { useToast } from "@/hooks/use-toast"
import { CreateAuthorDto } from "@/types"
import { Plus } from "lucide-react"

interface AddAuthorDialogProps {
    onAuthorCreated: () => void
}

export function AddAuthorDialog({ onAuthorCreated }: AddAuthorDialogProps) {
    const [open, setOpen] = useState(false)
    const [loading, setLoading] = useState(false)
    const { toast } = useToast()
    const [formData, setFormData] = useState<CreateAuthorDto>({
        firstName: "",
        lastName: "",
        biography: "",
    })

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setLoading(true)

        try {
            await authorsApi.create(formData)
            toast({
                title: "Sukces",
                description: "Autor został dodany",
            })
            setOpen(false)
            setFormData({ firstName: "", lastName: "", biography: "" })
            onAuthorCreated()
        } catch (error: any) {
            toast({
                title: "Błąd",
                description: error.response?.data?.message || "Nie udało się dodać autora",
                variant: "destructive",
            })
        } finally {
            setLoading(false)
        }
    }

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
                <Button variant="outline" size="icon" title="Dodaj nowego autora">
                    <Plus className="h-4 w-4" />
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>Dodaj autora</DialogTitle>
                    <DialogDescription>
                        Dodaj nowego autora do systemu.
                    </DialogDescription>
                </DialogHeader>
                <form onSubmit={handleSubmit}>
                    <div className="grid gap-4 py-4">
                        <div className="space-y-2">
                            <Label htmlFor="firstName">Imię *</Label>
                            <Input
                                id="firstName"
                                value={formData.firstName}
                                onChange={(e) =>
                                    setFormData({ ...formData, firstName: e.target.value })
                                }
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="lastName">Nazwisko *</Label>
                            <Input
                                id="lastName"
                                value={formData.lastName}
                                onChange={(e) =>
                                    setFormData({ ...formData, lastName: e.target.value })
                                }
                                required
                            />
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="biography">Biografia</Label>
                            <Textarea
                                id="biography"
                                value={formData.biography}
                                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                                    setFormData({ ...formData, biography: e.target.value })
                                }
                            />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => setOpen(false)}>
                            Anuluj
                        </Button>
                        <Button type="submit" disabled={loading}>
                            {loading ? "Dodawanie..." : "Dodaj"}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    )
}
