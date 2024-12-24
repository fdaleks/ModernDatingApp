import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../_services/user.service';

export const authGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const tostr = inject(ToastrService);

  if (userService.currentUser()) {
    return true;
  } else {
    tostr.error('You shall not pass!');
    return false;
  }
};
