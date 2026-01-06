export const colors = {
    discord: '#1f2936',
    discordPrimary: '#5c68ee',
    discordSecondary: '#36363b',
    discordWarning: '#c6383d',
    discordIcon: '#76777e',
    forestgreen: '#4d8e53',
} as const;

export type ColorKey = keyof typeof colors;