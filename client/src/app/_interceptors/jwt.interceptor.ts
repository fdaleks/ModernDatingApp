import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { UserService } from '../_services/user.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const userService = inject(UserService);

  if (userService.currentUser()) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${userService.currentUser()?.token}`
      }
    });
  }

  return next(req);
};
