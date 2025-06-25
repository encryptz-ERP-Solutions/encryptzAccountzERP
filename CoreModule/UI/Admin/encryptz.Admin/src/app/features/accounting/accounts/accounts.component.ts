import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CdkDrag, CdkDragEnd } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { SideBarComponent } from "./side-bar/side-bar.component";
import { BreakpointObserver } from '@angular/cdk/layout';


@Component({
  selector: 'app-accounts',
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
    SideBarComponent
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
  standalone: true
})
export class AccountsComponent {

  top = 0;
  left = 0;
  isDropped = false;

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

  onDragEnd(event: CdkDragEnd): void {
    const rect = (event.source.getRootElement() as HTMLElement).getBoundingClientRect();
    this.top = rect.top;
    this.left = rect.left;
    this.isDropped = true;
  }

  checkExpandNav(event : any){
    this.isSidebarExpanded = event
  }
}
