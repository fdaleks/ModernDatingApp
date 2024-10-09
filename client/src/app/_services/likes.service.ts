import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { LikesParams } from '../_models/params/likesParams';
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
  likesParams = signal<LikesParams>(new LikesParams());
  likesCache = new Map();
  
  toogleLike(targetUserId: number) {
    return this.httpClient.post(this.baseUrl + 'likes/' + targetUserId, {});
  }

  getUserLikes() {
    const response = this.likesCache.get(Object.values(this.likesParams()).join('-'));
    if (response) return setPaginatedResponse(this.paginatedResult, response);

    let params = setPaginationHeaders(this.likesParams().pageNumber, this.likesParams().pageSize);
    params = params.append('predicate', this.likesParams().predicate);

    return this.httpClient.get<Member[]>(this.baseUrl + 'likes', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(this.paginatedResult, response);
        this.likesCache.set(Object.values(this.likesParams()).join('-'), response);
      }
    });
  }

  getCurrentUserLikeIds() {
    return this.httpClient.get<number[]>(this.baseUrl + 'likes/list').subscribe({
      next: response => this.likeIds.set(response)
    });
  }

  clearLikesCache() {
    this.likesCache.clear();
  }

}
