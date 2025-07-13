import { CdkDrag } from '@angular/cdk/drag-drop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterOutlet } from '@angular/router';
import { AccountsSideBarComponent } from "./accounts-side-bar/accounts-side-bar.component";

@Component({
  selector: 'app-accounts-layout',
  imports: [
    RouterOutlet,
    CdkDrag,
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatSelectModule,
    AccountsSideBarComponent
],
  templateUrl: './accounts-layout.component.html',
  styleUrl: './accounts-layout.component.scss'
})
export class AccountsLayoutComponent {

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
