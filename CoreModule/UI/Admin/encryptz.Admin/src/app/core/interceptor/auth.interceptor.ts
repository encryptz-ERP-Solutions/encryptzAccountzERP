import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CommonService } from '../../shared/services/common.service';
import { finalize } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const commonService = inject(CommonService);
  const token = localStorage.getItem('access_token');

  const shouldShowLoader = !req.headers.has('no-loader');
  if (shouldShowLoader) {
    commonService.loaderState(true);
  }

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
      headers: req.headers.delete('no-loader')
    });
  } else {
    authReq = req.clone({
      headers: req.headers.delete('no-loader')
    });
  }

  return next(authReq).pipe(
    finalize(() => {
      if (shouldShowLoader) {
        commonService.loaderState(false);
      }
    })
  );


  // if (token) {

  //   // Check if token is expired
  //   // try {
  //   //   const payload = JSON.parse(atob(token.split('.')[1]));
  //   //   const expiry = payload.exp * 1000;
  //   //   if (Date.now() >= expiry) {
  //   //     console.error('Token is expired');
  //   //     localStorage.removeItem('access_token');
  //   //     // Redirect to login or refresh token 
  //   //     return next(req);
  //   //   }
  //   // } catch (error) {
  //   //   console.error('Invalid token format');
  //   //   localStorage.removeItem('access_token');
  //   //   return next(req);
  //   // }

  //   const authReq = req.clone({
  //     setHeaders: {
  //       Authorization: `Bearer ${token}`
  //     }
  //   });

  //   return next(authReq);
  // }
  // return next(req);
};
