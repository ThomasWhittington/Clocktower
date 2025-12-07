import '@testing-library/jest-dom';
import {
    render,
    screen
} from '@testing-library/react';
import {
    DiscordUserVoiceStatus
} from './DiscordUserVoiceStatus';
import type {
    VoiceState
} from '@/types/voiceState';

vi.mock('@/../public/icons/selfMuted.svg?react', () => ({
    default: () =>
        <div
            data-testid="self-muted-icon">SelfMuted</div>
}));
vi.mock('@/../public/icons/serverMuted.svg?react', () => ({
    default: () =>
        <div
            data-testid="server-muted-icon">ServerMuted</div>
}));
vi.mock('@/../public/icons/selfDeafened.svg?react', () => ({
    default: () =>
        <div
            data-testid="self-deafened-icon">SelfDeafened</div>
}));
vi.mock('@/../public/icons/serverDeafened.svg?react', () => ({
    default: () =>
        <div
            data-testid="server-deafened-icon">ServerDeafened</div>
}));
const baseVoiceState: VoiceState = {
    isSelfMuted: false,
    isServerMuted: false,
    isSelfDeafened: false,
    isServerDeafened: false,
};
describe('DiscordUserVoiceStatus', () => {


    it('renders container with correct CSS class', () => {
        const {container} = render(
            <DiscordUserVoiceStatus
                voiceState={baseVoiceState}/>);
        const voiceStatusDiv = container.querySelector('.discord-user-voice-status');
        expect(voiceStatusDiv).toBeInTheDocument();
        expect(voiceStatusDiv).toHaveClass('discord-user-voice-status');
    });

    it('shows self-muted icon when self-muted and not server-muted', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isSelfMuted: true
                }}/>);
        expect(screen.getByTestId('self-muted-icon')).toBeInTheDocument();
    });

    it('hides self-muted icon when both self-muted and server-muted', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isSelfMuted: true,
                    isServerMuted: true
                }}/>);
        expect(screen.queryByTestId('self-muted-icon')).not.toBeInTheDocument();
        expect(screen.getByTestId('server-muted-icon')).toBeInTheDocument();
    });

    it('shows server-muted icon when server-muted', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isServerMuted: true
                }}/>);
        expect(screen.getByTestId('server-muted-icon')).toBeInTheDocument();
    });

    it('shows self-deafened icon when self-deafened and not server-deafened', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isSelfDeafened: true
                }}/>);
        expect(screen.getByTestId('self-deafened-icon')).toBeInTheDocument();
    });

    it('hides self-deafened icon when both self-deafened and server-deafened', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isSelfDeafened: true,
                    isServerDeafened: true
                }}/>);
        expect(screen.queryByTestId('self-deafened-icon')).not.toBeInTheDocument();
        expect(screen.getByTestId('server-deafened-icon')).toBeInTheDocument();
    });

    it('shows multiple icons when applicable', () => {
        render(
            <DiscordUserVoiceStatus
                voiceState={{
                    ...baseVoiceState,
                    isSelfMuted: true,
                    isSelfDeafened: true
                }}/>);
        expect(screen.getByTestId('self-muted-icon')).toBeInTheDocument();
        expect(screen.getByTestId('self-deafened-icon')).toBeInTheDocument();
    });
});