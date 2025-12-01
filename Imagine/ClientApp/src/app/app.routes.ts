import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { LandingPage } from './Pages/landing-page/landing-page';
import { About } from './Pages/about/about';
import { AllProducts } from './Pages/all-products/all-products';
import { Contact } from './Pages/contact/contact';
import { Cart } from './Pages/cart/cart';
import { ProductDetails } from './Pages/product-details/product-details';
import { Login } from './Pages/login/login';
import { Register } from './Pages/register/register';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { AdminHome } from './Pages/Admin/admin-home/admin-home';
import { Category } from './Pages/Admin/category/category';
import { Products } from './Pages/Admin/products/products';
import { AddProduct } from './Pages/Admin/add-product/add-product';
import { Customers } from './Pages/Admin/customers/customers';
import { Orders } from './Pages/Admin/orders/orders';

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
            }
        ]
    },
     {
        path:"admin",
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
                path:"customers",
                component:Customers
            },
            {
                path:"orders",
                component:Orders
            }
         
        ]
    }
];
