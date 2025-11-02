export class ConverterUtils {

    static convertStringIdsToBigints = (obj: any): any => {
        if (obj === null || typeof obj !== 'object') {
            if (typeof obj === 'string' && /^\d{15,}$/.test(obj)) {
                return BigInt(obj);
            }
            return obj;
        }

        if (Array.isArray(obj)) {
            return obj.map(ConverterUtils.convertStringIdsToBigints);
        }

        const converted: any = {};
        for (const [key, value] of Object.entries(obj)) {
            converted[key] = ConverterUtils.convertStringIdsToBigints(value);
        }
        return converted;
    }
}