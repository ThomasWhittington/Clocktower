import {
    checkGuildApi
} from '../openApi';

async function checkGuild(id: bigint) {
    const {
        data,
        error
    } = await checkGuildApi({
        path: {
            guildId: id,
        }
    });

    if (error) {
        console.error('Failed to check guildId:', error);
        throw new Error(error.toString());
    }

    return data ?? {
        valid: false,
        name: '',
        message: ''
    };
}


export const discordService = {
    checkGuild,
}