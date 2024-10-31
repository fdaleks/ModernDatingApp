export interface User {
    id: number;
    userName: string;
    roles: string[];
    knownAs: string;
    gender: string;
    token: string;
    photoUrl?: string;
}