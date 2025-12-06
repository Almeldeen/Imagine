import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  const token = auth.getToken();
  if (!token) {
    router.navigate(['/Login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  if (!auth.hasRole('Admin')) {
    router.navigate(['/unauthorized']);
    return false;
  }

  return true;
};
