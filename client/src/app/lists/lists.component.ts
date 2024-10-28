import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { LikesService } from '../_services/likes.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { MemberCardComponent } from "../members/member-card/member-card.component";
import { PaginationModule } from 'ngx-bootstrap/pagination';

@Component({
  selector: 'app-lists',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './lists.component.html',
  styleUrl: './lists.component.css'
})
export class ListsComponent implements OnInit, OnDestroy {
  likesService = inject(LikesService);
  pageNumber: number = 1;
  pageSize: number = 4;
  predicate: string = 'liked';

  ngOnInit(): void {
    this.loadLikes();
  }

  ngOnDestroy(): void {
    this.likesService.paginatedResult.set(null);
  }

  loadLikes() {
    this.likesService.getUserLikes(this.pageNumber, this.pageSize, this.predicate);
  }

  getTitle() {
    switch (this.predicate) {
      case 'liked':
        return 'Members you like';
      case 'likedBy':
        return 'Members who like you';
      case 'mutual':
      default:
        return 'Mutual likes'
    }
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadLikes();
    }
  }

}
