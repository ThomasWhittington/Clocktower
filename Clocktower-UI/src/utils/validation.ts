export class ValidationUtils {
    static isValidDiscordId(id: bigint | null | undefined): id is bigint {
        return id !== null && id !== undefined && id > 41943040000n;
    }

    static validateDiscordId(id: bigint | null | undefined): { isValid: boolean; error?: string } {
        if (id === null || id === undefined) {
            return { isValid: false, error: "Discord ID not provided" };
        }

        if (id <= 41943040000n) {
            return { isValid: false, error: "Invalid Discord ID format" };
        }

        return { isValid: true };
    }
}