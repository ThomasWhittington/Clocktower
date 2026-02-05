import {
    type CSSProperties,
    useId
} from 'react';

interface HandIconProps {
    className?: string;
    style?: CSSProperties;
    gradientId?: string;
}

export const HandIcon = ({className, style, gradientId}: HandIconProps) => {
    const uniqueId = useId();
    const finalGradientId = gradientId ?? `handGradient-${uniqueId}`;

    return (
        <svg
            fill="none"
            stroke="currentColor"
            strokeWidth="16"
            viewBox="-32 0 512 512"
            xmlns="http://www.w3.org/2000/svg"
            className={className}
            style={style}
        >
            <defs>
                <linearGradient
                    id={finalGradientId}
                    x1="0%"
                    y1="0%"
                    x2="0%"
                    y2="100%"
                >
                    <stop offset="0%" stopColor="currentColor"/>
                    <stop offset="100%" stopColor="var(--gradient-end, #000000)"/>
                </linearGradient>
            </defs>
            <path
                fill={`url(#${finalGradientId})`}
                stroke="inherit"
                strokeWidth="inherit"
                d="M408.781 128.007C386.356 127.578 368 146.36 368 168.79V256h-8V79.79c0-22.43-18.356-41.212-40.781-40.783C297.488 39.423 280 57.169 280 79v177h-8V40.79C272 18.36 253.644-.422 231.219.007 209.488.423 192 18.169 192 40v216h-8V80.79c0-22.43-18.356-41.212-40.781-40.783C121.488 40.423 104 58.169 104 80v235.992l-31.648-43.519c-12.993-17.866-38.009-21.817-55.877-8.823-17.865 12.994-21.815 38.01-8.822 55.877l125.601 172.705A48 48 0 0 0 172.073 512h197.59c22.274 0 41.622-15.324 46.724-37.006l26.508-112.66a192.011 192.011 0 0 0 5.104-43.975V168c.001-21.831-17.487-39.577-39.218-39.993z"
            />
        </svg>
    );
};