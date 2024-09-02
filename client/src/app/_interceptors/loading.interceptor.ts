import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  busyService.busy();
  return next(req).pipe(
    // we use it to make a delay on the localhost
    // should be removed before publishing
    delay(1000),
    finalize(() => {
      busyService.idle();
    })
  );
};
