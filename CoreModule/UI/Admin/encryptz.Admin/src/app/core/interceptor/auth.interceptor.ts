import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');
  
  if (token) {
    // Check if token is expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      if (Date.now() >= expiry) {
        console.error('Token is expired');
        localStorage.removeItem('access_token');
        // Redirect to login or refresh token 
        return next(req);
      }
    } catch (error) {
      console.error('Invalid token format');
      localStorage.removeItem('access_token');
      return next(req);
    }
    
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    return next(authReq);
  }
  return next(req);
};
