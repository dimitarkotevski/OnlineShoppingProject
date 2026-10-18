export class RequestModel{
    SearchString?: string
    SortName?: string
    isAscending?: string
    PageNeeded?: number
    Size?: number

    // Grid filters; null / undefined / '' mean "any".
    /** Products: category id. */
    EntityId?: string | null
    /** Products: price, orders: total (inclusive). */
    PriceFrom?: number | null
    PriceTo?: number | null
    /** Products: created, orders: placed. 'yyyy-MM-dd' from a date input. */
    DateFrom?: string | null
    DateTo?: string | null
    HasImage?: boolean | null
    OrderStatus?: number | null
    PaymentType?: number | null
    /** Users: 1 = Admin, 2 = User (NexCart.Domain.Enums.UserRole). */
    Role?: number | null
    City?: string | null
    EmailEnding?: string | null
    /** Categories: active / inactive. */
    IsActive?: boolean | null
}

/** The filter fields, so a grid can clear them all while keeping paging and search. */
export const FILTER_KEYS: (keyof RequestModel)[] = [
    'EntityId', 'PriceFrom', 'PriceTo', 'DateFrom', 'DateTo', 'HasImage',
    'OrderStatus', 'PaymentType', 'Role', 'City', 'EmailEnding', 'IsActive',
];

/**
 * Copy of the request without empty values. Cleared inputs give '' (dates, text) or
 * null (numbers, selects); the API would reject '' for a date or number, so they are dropped.
 */
export function toApiRequest(request: RequestModel): RequestModel {
    const result: any = {};
    for (const [key, value] of Object.entries(request)) {
        if (value !== null && value !== undefined && value !== '') {
            result[key] = typeof value === 'string' ? value.trim() : value;
        }
    }
    return result;
}

/** Same limit as RequestModelValidator.MaxTextLength on the API. */
export const MAX_FILTER_TEXT = 100;

const isSet = (value: unknown) => value !== null && value !== undefined && value !== '';

/**
 * Client-side copy of the API's RequestModelValidator rules, so invalid filters are shown
 * next to the inputs instead of being sent. Keys match the filter field names.
 */
export function validateFilters(request: RequestModel): { [field: string]: string } {
    const errors: { [field: string]: string } = {};

    if (isSet(request.PriceFrom) && Number(request.PriceFrom) < 0) {
        errors['PriceFrom'] = 'Minimum must not be negative.';
    }
    if (isSet(request.PriceTo) && Number(request.PriceTo) < 0) {
        errors['PriceTo'] = 'Maximum must not be negative.';
    }
    if (!errors['PriceFrom'] && !errors['PriceTo'] && isSet(request.PriceFrom) && isSet(request.PriceTo)
        && Number(request.PriceTo) < Number(request.PriceFrom)) {
        errors['PriceTo'] = 'Maximum must be greater than or equal to the minimum.';
    }
    // 'yyyy-MM-dd' strings compare correctly as text.
    if (isSet(request.DateFrom) && isSet(request.DateTo) && request.DateTo! < request.DateFrom!) {
        errors['DateTo'] = "'To' date must be on or after the 'from' date.";
    }
    for (const field of ['SearchString', 'City', 'EmailEnding'] as (keyof RequestModel)[]) {
        const value = request[field];
        if (typeof value === 'string' && value.trim().length > MAX_FILTER_TEXT) {
            errors[field] = `Must be at most ${MAX_FILTER_TEXT} characters.`;
        }
    }
    return errors;
}

/** Number of filters currently set (shown on the Filters badge). */
export function activeFilterCount(request: RequestModel): number {
    return FILTER_KEYS.filter(key => {
        const value = request[key];
        return value !== null && value !== undefined && value !== '';
    }).length;
}
