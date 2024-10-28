import { User } from "./user";

export class UserParams {
    pageNumber: number = 1;
    pageSize: number = 4;
    minAge: number = 18;
    maxAge: number = 60;
    gender: string;
    orderBy: string = 'lastActive';

    constructor(user: User | null) {
        this.gender = user?.gender === 'female' ? 'male' : 'female';
    }
}