import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { LandingPage } from './Pages/landing-page/landing-page';
import { About } from './Pages/about/about';
import { AllProducts } from './Pages/all-products/all-products';
import { Contact } from './Pages/contact/contact';
import { Cart } from './Pages/cart/cart';
import { Checkout } from './Pages/checkout/checkout';
import { ProductDetails } from './Pages/product-details/product-details';
import { Login } from './Pages/login/login';
import { Register } from './Pages/register/register';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { AdminHome } from './Pages/Admin/admin-home/admin-home';
import { AdminAnalytics } from './Pages/Admin/analytics/admin-analytics';
import { Category } from './Pages/Admin/category/category';
import { Products } from './Pages/Admin/products/products';
import { AddProduct } from './Pages/Admin/add-product/add-product';
import { AdminProductView } from './Pages/Admin/products/view-product/admin-product-view';
import { AdminEditProduct } from './Pages/Admin/products/edit-product/admin-edit-product';
import { Customers } from './Pages/Admin/customers/customers';
import { Orders } from './Pages/Admin/orders/orders';
import { Unauthorized } from './Pages/unauthorized/unauthorized';
import { NotFound } from './Pages/not-found/not-found';
import { authGuard } from './core/auth.guard';
import { adminGuard } from './core/admin.guard';
import { clientGuard } from './core/client.guard';

export const routes: Routes = [
    {
        path:"",
        component:MainLayout,
        children:[
            {
                path:"",
                pathMatch:"full",
                redirectTo:"Home"
            },
            {
                path:"Home",
                component:LandingPage
            },
            {
                path:"About",
                component:About
            },
            {
                path:"Products",
                component:AllProducts
            },
            {
                path:"Product/:id",
                component:ProductDetails
            },
            {
                path:"Login",
                component:Login
            },
            {
                path:"Register",
                component:Register
            },
            {
                path:"Contact",
                component:Contact
            },
            {
                path:"Cart",
                component:Cart
            },
            {
                path:"Checkout",
                component:Checkout,
                canActivate:[authGuard]
            }
        ]
    },
    {
        path: 'client',
        canActivate: [clientGuard],
        loadChildren: () => import('./Pages/Client/client.routes').then(m => m.CLIENT_ROUTES)
    },
    {
        path:"admin",
        canActivate: [adminGuard],
        component:AdminLayout,
        children:[
            {
                path:"",
                pathMatch:"full",
                redirectTo:"Home"
            },
            {
                path:"Home",
                component:AdminHome
            },
            {
                path:"analytics",
                component:AdminAnalytics
            },
            {
                path:"categories",
                component:Category
            },
            {
                path:"products",
                component:Products
            },
            {
                path:"products/add",
                component:AddProduct
            },
            {
                path:"products/view/:id",
                component:AdminProductView
            },
            {
                path:"products/edit/:id",
                component:AdminEditProduct
            },
            {
                path:"customers",
                component:Customers
            },
            {
                path:"orders",
                component:Orders
            },
            {
                path:"orders/pending",
                component:Orders,
                data:{ defaultStatus:'pending' }
            },
            {
                path:"orders/completed",
                component:Orders,
                data:{ defaultStatus:'delivered' }
            }
         
        ]
    },
    {
        path: 'unauthorized',
        component: Unauthorized
    },
    {
        path: '**',
        component: NotFound
    }
];
