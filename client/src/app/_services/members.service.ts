import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from '../_helpers/paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private httpClient = inject(HttpClient);
  private accountService = inject(AccountService);
  private baseUrl: string = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));
  memberCache = new Map();

  resetUserParams() {
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if (response) return setPaginatedResponse(this.paginatedResult, response);

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.httpClient.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(this.paginatedResult, response);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
    });
  }

  getMember(userName: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((x: Member) => x.userName === userName);

    if (member) return of(member);

    return this.httpClient.get<Member>(this.baseUrl + 'users/' + userName);
  }

  updateMember(member: Member) {
    return this.httpClient.put(this.baseUrl + 'users', member).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(x => x.userName === member.userName ? member : x));
      // })
    );
  }

  setMainPhoto(photo: Photo) {
    return this.httpClient.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(x => {
      //     if (x.photos.includes(photo)) {
      //       x.photoUrl = photo.url;
      //     }
      //     return x;
      //   }))
      // })
    );
  }

  deletePhoto(photo: Photo) {
    return this.httpClient.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(x => {
      //     if (x.photos.includes(photo)) {
      //       x.photos = x.photos.filter(p => p.id !== photo.id)
      //     }
      //     return x;
      //   }))
      // })
    );
  }

}
