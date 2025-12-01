import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminNavbar } from "./admin-navbar/admin-navbar";
import { AdminSidebar } from "./admin-sidebar/admin-sidebar";

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, AdminNavbar, AdminSidebar],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout {

}
