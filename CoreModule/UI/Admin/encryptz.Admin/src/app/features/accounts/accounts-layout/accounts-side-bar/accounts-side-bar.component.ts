import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NavigationEnd, Router } from '@angular/router';
import { filter, finalize, of, switchMap } from 'rxjs';
import { CommonService } from '../../../../shared/services/common.service';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { MenuTreeNode } from '../../../../core/models/admin.models';

interface AccountsMenuItem {
  icon: string;
  label: string;
  route?: string;
  disabled?: boolean;
  items: AccountsMenuItem[];
}

@Component({
  selector: 'app-accounts-side-bar',
  imports: [
    MatIconModule,
    MatButtonModule,
    CommonModule,
    MatDividerModule,
    MatExpansionModule,
    MatTooltipModule
  ],
  templateUrl: './accounts-side-bar.component.html',
  styleUrl: './accounts-side-bar.component.scss'
})
export class AccountsSideBarComponent {
  @Input() isExpand: boolean = false;
  @Output() isOpenSideNav = new EventEmitter<boolean>();
  selectedMenu: string | null = null;
  menuItems: AccountsMenuItem[] = [];
  loadingMenu = false;

  constructor(
    private router: Router,
    private commonService: CommonService,
    private adminDataService: AdminDataService
  ) { }

  ngOnInit() {
    this.selectedMenu = localStorage.getItem('currentAccountsMenu');
    this.loadMenu();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.selectedMenu = event.urlAfterRedirects || event.url;
        localStorage.setItem('currentAccountsMenu', this.selectedMenu ?? '');
      });
  }

  private loadMenu(): void {
    this.loadingMenu = true;
    this.commonService.loaderState(true);

    this.adminDataService.getModules()
      .pipe(
        switchMap(modules => {
          const accountsModule = modules.find(m => 
            m.moduleName?.toLowerCase() === 'accounts' || 
            m.moduleName?.toLowerCase().includes('account')
          );
          if (!accountsModule) {
            return of<AccountsMenuItem[]>(this.getFallbackMenu());
          }
          return this.adminDataService.getMenuItemsByModule(accountsModule.moduleID)
            .pipe(
              switchMap(menuItems => {
                const tree = this.adminDataService.buildMenuTree(menuItems);
                return of(this.mapTreeToNav(tree));
              })
            );
        }),
        finalize(() => {
          this.loadingMenu = false;
          this.commonService.loaderState(false);
        })
      )
      .subscribe({
        next: items => {
          this.menuItems = (items && items.length > 0)
            ? items
            : this.getFallbackMenu();
        },
        error: () => {
          this.menuItems = this.getFallbackMenu();
        }
      });
  }

  private mapTreeToNav(nodes: MenuTreeNode[]): AccountsMenuItem[] {
    return nodes.map(node => ({
      icon: node.iconClass || 'menu',
      label: node.menuText,
      route: node.menuURL || undefined,
      disabled: !node.isActive || !node.menuURL,
      items: this.mapTreeToNav(node.children)
    }));
  }

  private getFallbackMenu(): AccountsMenuItem[] {
    return [
      { icon: 'dashboard', label: 'Workspace Overview', route: '/accounts/dashboard', items: [] },
      { icon: 'account_tree', label: 'Chart of Accounts', route: '/accounts/chartofaccounts', items: [] },
      { icon: 'receipt_long', label: 'Vouchers', disabled: true, items: [] },
      { icon: 'library_books', label: 'Ledger Reports', disabled: true, items: [] },
      { icon: 'analytics', label: 'Trial Balance', disabled: true, items: [] }
    ];
  }

  openSideNav() {
    this.isOpenSideNav.emit(true);
  }

  closeSideNav() {
    this.isOpenSideNav.emit(false);
  }

  navigateMenu(item: AccountsMenuItem) {
    if (item.disabled || !item.route) {
      this.commonService.showSnackbar('Feature coming soon.', 'INFO', 2500);
      return false;
    }

    this.router.navigateByUrl(item.route);
    this.selectedMenu = item.route;
    localStorage.setItem('currentAccountsMenu', this.selectedMenu ?? '');
    return false;
  }
}
