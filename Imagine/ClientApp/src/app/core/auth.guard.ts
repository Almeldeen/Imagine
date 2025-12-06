import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  if (typeof window === 'undefined') {
    return false;
  }

  const token = localStorage.getItem('authToken');
  if (token) {
    return true;
  }

  const router = inject(Router);
  router.navigate(['/Login'], {
    queryParams: { returnUrl: state.url },
  });
  return false;
};
