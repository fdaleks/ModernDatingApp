import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../_services/user.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const tostr = inject(ToastrService);

  if (userService.roles().includes('Admin') || userService.roles().includes('Moderator')) {
    return true;
  } else {
    tostr.error('You can\'t enter this area');
    return false;
  }
};
