import type {
    Transition,
    Variant
} from "framer-motion";

export const transitions = {
    fast: {duration: 0.15, ease: "easeInOut"},
    normal: {duration: 0.3, ease: "easeInOut"},
    slow: {duration: 0.5, ease: "easeInOut"},
    spring: {type: "spring", stiffness: 300, damping: 20},
} as const satisfies Record<string, Transition>;

export const animations = {
    zoomIn: {
        initial: {scale: 0.5, opacity: 0},
        animate: {scale: 1, opacity: 1},
        exit: {scale: 0.5, opacity: 0},
        transition: transitions.fast,
    },

    zoomInSpring: {
        initial: {scale: 0.5, opacity: 0},
        animate: {scale: 1, opacity: 1},
        exit: {scale: 0.5, opacity: 0},
        transition: transitions.spring,
    },

    fade: {
        initial: {opacity: 0},
        animate: {opacity: 1},
        exit: {opacity: 0},
        transition: transitions.normal,
    },

    slideUp: {
        initial: {y: 20, opacity: 0},
        animate: {y: 0, opacity: 1},
        exit: {y: -20, opacity: 0},
        transition: transitions.normal,
    },

    slideDown: {
        initial: {y: -20, opacity: 0},
        animate: {y: 0, opacity: 1},
        exit: {y: 20, opacity: 0},
        transition: transitions.normal,
    },

    slideLeft: {
        initial: {x: 20, opacity: 0},
        animate: {x: 0, opacity: 1},
        exit: {x: -20, opacity: 0},
        transition: transitions.normal,
    },

    slideRight: {
        initial: {x: -20, opacity: 0},
        animate: {x: 0, opacity: 1},
        exit: {x: 20, opacity: 0},
        transition: transitions.normal,
    },

    scaleIn: {
        initial: {scale: 0.8, opacity: 0},
        animate: {scale: 1, opacity: 1},
        exit: {scale: 0.8, opacity: 0},
        transition: transitions.normal,
    },

    rotateIn: {
        initial: {rotate: -10, scale: 0.8, opacity: 0},
        animate: {rotate: 0, scale: 1, opacity: 1},
        exit: {rotate: 10, scale: 0.8, opacity: 0},
        transition: transitions.normal,
    },

    flip: {
        initial: {rotateY: -90, opacity: 0},
        animate: {rotateY: 0, opacity: 1},
        exit: {rotateY: 90, opacity: 0},
        transition: {duration: 0.3, ease: "easeInOut" as const},
        style: {willChange: "transform, opacity", perspective: 1000}
    }
} as const;

export const createAnimation = (
    initial: Variant,
    animate: Variant,
    exit: Variant,
    transition: Transition = transitions.normal
) => ({
    initial,
    animate,
    exit,
    transition,
});