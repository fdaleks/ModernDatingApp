import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from '../_helpers/paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;
  likeIds = signal<number[]>([]);
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  
  toogleLike(targetUserId: number) {
    return this.httpClient.post(this.baseUrl + 'likes/' + targetUserId, {});
  }

  getUserLikes(pageNumber: number, pageSize: number, predicate: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('predicate', predicate);

    return this.httpClient.get<Member[]>(this.baseUrl + 'likes', { observe: 'response', params }).subscribe({
      next: response => setPaginatedResponse(this.paginatedResult, response)
    });
  }

  getCurrentUserLikeIds() {
    return this.httpClient.get<number[]>(this.baseUrl + 'likes/list').subscribe({
      next: response => this.likeIds.set(response)
    });
  }

}
