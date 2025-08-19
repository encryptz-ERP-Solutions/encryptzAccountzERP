import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule, Location } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { SideBarComponent } from "./side-bar/side-bar.component";
import { filter, map, Observable, startWith, switchMap } from 'rxjs';

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

  public screenTitle$ !: Observable<string>;
  screenTitle : string = ''
  isSmallDevice: boolean = false
  isSidebarExpanded: boolean = true;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    public location : Location
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

  ngOnInit(): void {
    this.setTitle();
  }

  setTitle() {
    this.screenTitle$ = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      startWith(null), // Emit a null event initially to trigger the title calculation
      map(() => this.activatedRoute),
      map(route => {
        while (route.firstChild) {
          route = route.firstChild;
        }
        return route;
      }),
      switchMap(route => route.data),
      map(data => data['title'])
    );

    this.screenTitle$.subscribe(title => {
      if (title) {
        this.screenTitle = title;
      }
    });
  }

  toggleSidebar() {
    this.isSidebarExpanded = !this.isSidebarExpanded;
  }
  checkExpandNav(event: any) {
    this.isSidebarExpanded = event
  }

}
