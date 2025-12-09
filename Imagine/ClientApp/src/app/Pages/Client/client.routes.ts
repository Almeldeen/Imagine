import { Routes } from '@angular/router';
import { UserLayout } from '../../layout/user-layout/user-layout';
import { ClientDashboard } from './dashboard/client-dashboard';
import { ClientSection } from './section/client-section';
import { ClientProfile } from './profile/client-profile';
import { ClientCustomization } from './customization/client-customization';
import { ClientOrders } from './orders/client-orders';
import { ClientOrderConfirmation } from './orders/client-order-confirmation';
import { authGuard } from '../../core/auth.guard';

export const CLIENT_ROUTES: Routes = [
  {
    path: '',
    component: UserLayout,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        component: ClientDashboard,
      },
      {
        path: 'orders',
        component: ClientOrders,
      },
      {
        path: 'orders/confirmation/:id',
        component: ClientOrderConfirmation,
      },
      {
        path: 'profile',
        component: ClientProfile,
      },
      {
        path: 'addresses',
        component: ClientSection,
        data: {
          title: 'Addresses',
          description: 'Manage your shipping and billing addresses.',
        },
      },
      {
        path: 'wishlist',
        component: ClientSection,
        data: {
          title: 'Wishlist',
          description: 'View and manage products you have saved.',
        },
      },
      {
        path: 'customize',
        component: ClientCustomization,
        data: {
          title: 'AI customization studio',
          description: 'Design AI-powered prints for hoodies and T-shirts.',
        },
      },
    ],
  },
];
