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

  ngOnInit(): void {
    if (!this.likesService.paginatedResult()) this.loadLikes();
  }

  // pls note that we clear cache when user switch bewtween pages
  ngOnDestroy(): void {
    this.likesService.paginatedResult.set(null);
    this.likesService.clearLikesCache();
  }

  loadLikes() {
    this.likesService.getUserLikes();
  }

  getTitle() {
    switch (this.likesService.likesParams().predicate) {
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
    if (this.likesService.likesParams().pageNumber !== event.page) {
      this.likesService.likesParams().pageNumber = event.page;
      this.loadLikes();
    }
  }

}
