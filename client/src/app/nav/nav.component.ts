import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { AccountService } from '../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { LikesService } from '../_services/likes.service';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  public accountService = inject(AccountService);
  private likesService = inject(LikesService);
  private router = inject(Router);
  private tostr = inject(ToastrService);
  loginModel: any = {};

  login() {
    this.accountService.login(this.loginModel).subscribe({
      next: () => this.router.navigateByUrl('/members'),
      error: error => this.tostr.error(error.error)
    });
  }

  logout() {
    this.accountService.logout();
    this.likesService.clearLikesCache();
    this.router.navigateByUrl('/');
  }
}
