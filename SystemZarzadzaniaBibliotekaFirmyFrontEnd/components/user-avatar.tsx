"use client"

import Avatar from "boring-avatars"
import { cn } from "@/lib/utils"

interface UserAvatarProps {
  userId: number
  username?: string
  email?: string
  className?: string
  size?: number
  variant?: "marble" | "beam" | "pixel" | "sunset" | "ring" | "bauhaus"
}

export function UserAvatar({ 
  userId, 
  username, 
  email, 
  className, 
  size = 32,
  variant = "marble"
}: UserAvatarProps) {
  // Użyj nazwy użytkownika, email lub ID jako seed dla avatara
  const name = username || email || `user-${userId}`

  return (
    <div
      className={cn(
        "flex items-center justify-center rounded-full overflow-hidden",
        className
      )}
      style={{ width: size, height: size }}
    >
      <Avatar
        name={name}
        size={size}
        variant={variant}
        square={false}
      />
    </div>
  )
}

