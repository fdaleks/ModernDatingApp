import { User } from "../user";
import { PaginationParams } from "./paginationParams";

export class UserParams extends PaginationParams {
    minAge: number = 18;
    maxAge: number = 60;
    gender: string;
    orderBy: string = 'lastActive';

    constructor(user: User | null) {
        super();
        this.gender = user?.gender === 'female' ? 'male' : 'female';
    }
}