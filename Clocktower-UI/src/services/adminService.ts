import {cancelTimerApi, healthApi, startOrEditTimerApi} from "@/api";
import {apiClient} from "@/api/api-client.ts";

async function health() {
    const {
        data,
        error
    } = await healthApi({client: apiClient});

    if (error) {
        console.error('Failed to verify health of server:', error);
        throw new Error(error.toString());
    }

    return {
        status: data?.status!,
        timeStamp: data?.timeStamp!
    };
}

async function startOrEditTimer(gameId: string, durationSeconds: number, label?: string) {
    const {
        data,
        error
    } = await startOrEditTimerApi({
        client: apiClient,
        path: {
            gameId: gameId
        },
        body: {
            durationSeconds: durationSeconds,
            label: label
        }
    });

    if (error) {
        console.error('Failed to start or edit timer:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function cancelTimer(gameId: string) {
    const {
        data,
        error
    } = await cancelTimerApi({
        client: apiClient,
        path: {
            gameId: gameId
        }
    });

    if (error) {
        console.error('Failed to cancel timer:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

const getMessage = (err: unknown): string => {
    if (typeof err === "string") return err;
    if (err instanceof Error) return err.message;
    if (typeof err === "object" && err && typeof (err as any).message === "string") {
        return (err as any).message;
    }
    return "Unknown error";
};

export const adminService = {
    health,
    startOrEditTimer,
    cancelTimer
}