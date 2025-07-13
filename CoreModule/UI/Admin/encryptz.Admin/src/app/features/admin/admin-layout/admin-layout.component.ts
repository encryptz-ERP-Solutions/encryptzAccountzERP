import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterOutlet } from '@angular/router';
import { SideBarComponent } from "./side-bar/side-bar.component";

@Component({
  selector: 'app-admin-layout',
  imports: [
    RouterOutlet,
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSidenavModule,
    SideBarComponent
],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  standalone: true
})
export class AdminLayoutComponent {


  isSmallDevice: boolean = false
  isSidebarExpanded: boolean = true;

  constructor(private breakpointObserver: BreakpointObserver
  ) {
    this.breakpointObserver.observe(['(max-width: 767.99px)']).subscribe(result => {
      if (result.matches) {
        this.isSmallDevice = true
      }
      else {
        this.isSmallDevice = false
      }
    })
  }

  toggleSidebar() {
    this.isSidebarExpanded = !this.isSidebarExpanded;
  }
  checkExpandNav(event: any) {
    this.isSidebarExpanded = event
  }

}
