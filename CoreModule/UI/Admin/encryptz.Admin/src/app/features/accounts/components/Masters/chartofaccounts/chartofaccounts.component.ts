import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from "@angular/material/card";
import { MatTreeModule, MatTreeNestedDataSource } from '@angular/material/tree';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { NestedTreeControl } from '@angular/cdk/tree';
import { finalize, Subscription } from 'rxjs';
import { AddEditChartOfAccountComponent } from './add-edit-chart-of-account/add-edit-chart-of-account.component';
import { ChartOfAccountsService, ChartOfAccount } from '../../../../../core/services/chart-of-accounts.service';
import { CommonService } from '../../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../../shared/components/confirmation/confirmation.component';
import { BusinessContextService } from '../../../../../core/services/business-context.service';

export interface AccountTreeNode {
  account: ChartOfAccount;
  children: AccountTreeNode[];
  level: number;
}

@Component({
  selector: 'app-chartofaccounts',
  standalone: true,
  imports: [
    CommonModule,
    MatTreeModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressBarModule,
    ReactiveFormsModule
  ],
  templateUrl: './chartofaccounts.component.html',
  styleUrl: './chartofaccounts.component.scss'
})
export class ChartofaccountsComponent implements OnInit, OnDestroy {
  treeControl = new NestedTreeControl<AccountTreeNode>(
    (node: AccountTreeNode) => {
      const children = node.children || [];
      if (children.length > 0) {
        console.log(`TreeControl: Getting ${children.length} children for ${node.account.accountCode} (${node.account.accountName})`);
      }
      return children;
    }
  );
  dataSource = new MatTreeNestedDataSource<AccountTreeNode>();
  filterControl = new FormControl('');
  loading = false;
  error?: string;
  accountTypes: Map<number, string> = new Map();
  allAccounts: ChartOfAccount[] = [];
  private subscriptions = new Subscription();
  
  hasChild = (_: number, node: AccountTreeNode): boolean => {
    const children = node.children || [];
    const hasChildren = Array.isArray(children) && children.length > 0;
    if (hasChildren) {
      console.log(`Node ${node.account.accountCode} (${node.account.accountName}) has ${children.length} children:`, 
        children.map(c => `${c.account.accountCode} - ${c.account.accountName}`));
    } else {
      console.log(`Node ${node.account.accountCode} (${node.account.accountName}) has NO children`);
    }
    return hasChildren;
  };

  constructor(
    private dialog: MatDialog,
    private chartOfAccountsService: ChartOfAccountsService,
    private commonService: CommonService,
    private businessContext: BusinessContextService,
    private cdr: ChangeDetectorRef
  ) {
    this.dataSource.data = [];
  }

  ngOnInit() {
    // Subscribe to business changes
    this.subscriptions.add(
      this.businessContext.selectedBusiness$.subscribe(business => {
        if (business) {
          this.error = undefined;
          this.loadAccountTypes();
          this.loadAccounts();
        } else {
          this.error = 'Please select a business to view chart of accounts';
          this.dataSource.data = [];
          this.allAccounts = [];
        }
      })
    );

    this.subscriptions.add(
      this.filterControl.valueChanges.subscribe(value => {
        this.applyFilter(value || '');
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadAccounts(): void {
    const business = this.businessContext.currentBusiness;
    if (!business) {
      this.error = 'Please select a business to view chart of accounts';
      this.dataSource.data = [];
      this.allAccounts = [];
      return;
    }

    this.loading = true;
    this.error = undefined;
    this.commonService.loaderState(true);
    
    const businessId = business.businessID;
    console.log('Loading accounts for business:', {
      businessId: businessId,
      businessIdType: typeof businessId,
      businessIdLength: businessId?.length,
      business: business
    });
    
    // Validate businessId format (should be a GUID)
    if (!businessId || businessId.trim() === '') {
      this.error = 'Invalid business ID';
      this.commonService.showSnackbar('Invalid business ID', 'ERROR', 3000);
      this.loading = false;
      this.commonService.loaderState(false);
      return;
    }
    
    this.chartOfAccountsService.getAll(businessId)
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: accounts => {
          console.log('=== Accounts Loaded from API ===');
          console.log('Total accounts:', accounts.length);
          
          // Log all account IDs and their parent IDs
          console.log('\nAll Account IDs and Parent IDs:');
          accounts.forEach(acc => {
            console.log(`  ${acc.accountCode} (${acc.accountName})`);
            console.log(`    accountID: ${acc.accountID} (type: ${typeof acc.accountID})`);
            console.log(`    parentAccountID: ${acc.parentAccountID || 'NULL'} (type: ${typeof acc.parentAccountID})`);
          });
          
          console.log('\nAccounts with parentAccountID:');
          const accountsWithParent = accounts.filter(a => a.parentAccountID);
          console.log(`Count: ${accountsWithParent.length}`);
          accountsWithParent.slice(0, 10).forEach(acc => {
            console.log(`  ${acc.accountCode} - ${acc.accountName}`);
            console.log(`    ID: ${acc.accountID}, Parent ID: ${acc.parentAccountID}`);
            // Check if parent exists in the accounts list
            const parentExists = accounts.some(a => a.accountID === acc.parentAccountID);
            console.log(`    Parent exists in list: ${parentExists}`);
          });
          
          console.log('\nRoot accounts (no parent):');
          accounts.filter(a => !a.parentAccountID).forEach(acc => {
            console.log(`  ${acc.accountCode} - ${acc.accountName} (ID: ${acc.accountID})`);
          });
          console.log('================================\n');
          // Map account type names if available
          this.allAccounts = accounts.map(acc => ({
            ...acc,
            accountTypeName: this.accountTypes.get(acc.accountTypeID) || acc.accountTypeName || '—'
          }));
          
          // Build tree structure
          const treeData = this.buildTree(this.allAccounts);
          console.log('Setting tree data:', treeData);
          console.log('Tree data structure:', JSON.stringify(treeData.map(n => ({
            code: n.account.accountCode,
            name: n.account.accountName,
            childrenCount: n.children.length,
            children: n.children.map(c => c.account.accountCode)
          })), null, 2));
          
          // Create a new array reference to trigger change detection
          // IMPORTANT: Material Tree needs a fresh array reference
          this.dataSource.data = null as any;
          this.cdr.detectChanges();
          this.dataSource.data = treeData;
          this.cdr.detectChanges();
          
          console.log('DataSource set, verifying tree structure:');
          treeData.forEach(rootNode => {
            console.log(`Root: ${rootNode.account.accountCode} - Children: ${rootNode.children.length}`);
            rootNode.children.forEach(child => {
              console.log(`  Child: ${child.account.accountCode} - Children: ${child.children.length}`);
            });
          });
          
          // Expand all nodes by default
          if (treeData.length > 0) {
            // Use setTimeout to ensure tree is rendered before expanding
            setTimeout(() => {
              console.log('Expanding all nodes');
              this.treeControl.expandAll();
              this.cdr.detectChanges();
              
              // Verify expansion and children recursively
              const verifyNode = (node: AccountTreeNode, level: number = 0): void => {
                const indent = '  '.repeat(level);
                const expanded = this.treeControl.isExpanded(node);
                const hasChildren = this.hasChild(0, node);
                console.log(`${indent}${node.account.accountCode}: expanded=${expanded}, hasChild=${hasChildren}, childrenCount=${node.children.length}`);
                if (node.children && node.children.length > 0) {
                  node.children.forEach(child => verifyNode(child, level + 1));
                }
              };
              
              console.log('=== Tree Verification ===');
              treeData.forEach(node => verifyNode(node));
              console.log('========================');
            }, 300);
          }
          
          if (accounts.length === 0) {
            this.error = 'No accounts found for this business. Accounts may need to be created or seeded.';
          }
        },
        error: err => {
          console.error('Error loading accounts:', {
            error: err,
            errorMessage: err?.message,
            errorStatus: err?.status,
            errorStatusText: err?.statusText,
            errorUrl: err?.url,
            businessId: businessId,
            business: business
          });
          const errorMsg = err?.message || `Unable to load chart of accounts. Status: ${err?.status || 'Unknown'}`;
          this.error = errorMsg;
          this.commonService.showSnackbar(errorMsg, 'ERROR', 3000);
          this.dataSource.data = [];
          this.allAccounts = [];
        }
      });
  }

  openCreateDialog(): void {
    const business = this.businessContext.currentBusiness;
    if (!business || !business.businessID) {
      this.commonService.showSnackbar('Please select a business first', 'ERROR', 3000);
      return;
    }

    this.chartOfAccountsService.getAll(business.businessID).subscribe({
      next: accounts => {
        const dialogRef = this.dialog.open(AddEditChartOfAccountComponent, {
          width: '600px',
          data: { 
            mode: 'create',
            parentAccounts: accounts.filter(acc => !acc.isSystemAccount)
          }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.loadAccounts();
          }
        });
      },
      error: () => {
        const dialogRef = this.dialog.open(AddEditChartOfAccountComponent, {
          width: '600px',
          data: { mode: 'create' }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.loadAccounts();
          }
        });
      }
    });
  }

  openEditDialog(account: ChartOfAccount): void {
    if (account.isSystemAccount) {
      this.commonService.showSnackbar('System accounts cannot be edited', 'INFO', 3000);
      return;
    }

    const business = this.businessContext.currentBusiness;
    if (!business || !business.businessID) {
      this.commonService.showSnackbar('Please select a business first', 'ERROR', 3000);
      return;
    }

    this.chartOfAccountsService.getAll(business.businessID).subscribe({
      next: accounts => {
        const dialogRef = this.dialog.open(AddEditChartOfAccountComponent, {
          width: '600px',
          data: { 
            mode: 'edit',
            account,
            parentAccounts: accounts.filter(acc => 
              !acc.isSystemAccount && acc.accountID !== account.accountID
            )
          }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.loadAccounts();
          }
        });
      },
      error: () => {
        const dialogRef = this.dialog.open(AddEditChartOfAccountComponent, {
          width: '600px',
          data: { mode: 'edit', account }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.loadAccounts();
          }
        });
      }
    });
  }

  deleteAccount(account: ChartOfAccount): void {
    if (account.isSystemAccount) {
      this.commonService.showSnackbar('System accounts cannot be deleted', 'INFO', 3000);
      return;
    }

    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete account',
        description: `Are you sure you want to delete "${account.accountName}" (${account.accountCode})? This action cannot be undone.`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.chartOfAccountsService.delete(account.accountID).subscribe({
          next: () => {
            this.commonService.showSnackbar('Account deleted successfully', 'SUCCESS', 3000);
            this.loadAccounts();
          },
          error: err => {
            this.commonService.showSnackbar(err?.message || 'Unable to delete account', 'ERROR', 3000);
          }
        });
      }
    });
  }

  applyFilter(value: string): void {
    const filterValue = value.trim().toLowerCase();
    if (!filterValue) {
      this.dataSource.data = this.buildTree(this.allAccounts);
      this.treeControl.expandAll();
      return;
    }

    // Filter accounts
    const filteredAccounts = this.allAccounts.filter(acc => {
      const term = filterValue.toLowerCase();
      const haystack = [
        acc.accountCode,
        acc.accountName,
        acc.description || '',
        this.getAccountTypeName(acc)
      ]
        .filter(Boolean)
        .map(v => v!.toLowerCase());
      return haystack.some(v => v.includes(term));
    });

    // Build tree with filtered accounts (include parents of filtered items)
    const filteredIds = new Set(filteredAccounts.map(a => a.accountID));
    const accountsToShow = new Set<string>();
    
    // Add filtered accounts and their parents
    filteredAccounts.forEach(acc => {
      accountsToShow.add(acc.accountID);
      let current = acc;
      while (current.parentAccountID) {
        const parent = this.allAccounts.find(a => a.accountID === current.parentAccountID);
        if (parent) {
          accountsToShow.add(parent.accountID);
          current = parent;
        } else {
          break;
        }
      }
    });

    const filtered = this.allAccounts.filter(a => accountsToShow.has(a.accountID));
    const treeData = this.buildTree(filtered);
    this.dataSource.data = [...treeData];
    this.treeControl.dataNodes = this.dataSource.data;
    if (treeData.length > 0) {
      setTimeout(() => {
        this.treeControl.expandAll();
      }, 0);
    }
  }

  refresh(): void {
    this.loadAccounts();
  }

  get hasAccounts(): boolean {
    return this.dataSource.data.length > 0;
  }

  loadAccountTypes(): void {
    this.chartOfAccountsService.getAccountTypes().subscribe({
      next: types => {
        types.forEach(type => {
          this.accountTypes.set(type.accountTypeID, type.accountTypeName);
        });
        // Reload accounts to update account type names
        if (this.allAccounts.length > 0) {
          this.allAccounts = this.allAccounts.map(acc => ({
            ...acc,
            accountTypeName: this.accountTypes.get(acc.accountTypeID) || acc.accountTypeName || '—'
          }));
          const treeData = this.buildTree(this.allAccounts);
          this.dataSource.data = [...treeData];
          this.treeControl.dataNodes = this.dataSource.data;
          if (treeData.length > 0) {
            setTimeout(() => {
              this.treeControl.expandAll();
            }, 0);
          }
        }
      },
      error: () => {
        // Silently fail - account types are optional
      }
    });
  }

  getAccountTypeName(account: ChartOfAccount): string {
    return this.accountTypes.get(account.accountTypeID) || account.accountTypeName || '—';
  }

  expandAll(): void {
    console.log('Expand all called, dataSource.data:', this.dataSource.data);
    console.log('Tree control dataNodes:', this.treeControl.dataNodes);
    this.treeControl.expandAll();
    // Log expanded state
    this.dataSource.data.forEach(node => {
      console.log(`Node ${node.account.accountCode} expanded: ${this.treeControl.isExpanded(node)}, has children: ${this.hasChild(0, node)}`);
    });
  }

  collapseAll(): void {
    this.treeControl.collapseAll();
  }

  private normalizeId(id: string | null | undefined): string {
    if (!id) return '';
    return String(id).toLowerCase().trim().replace(/-/g, '');
  }

  private findNodeById(accountMap: Map<string, AccountTreeNode>, searchId: string): AccountTreeNode | undefined {
    // Try exact match first
    let node = accountMap.get(searchId);
    if (node) return node;
    
    // Try normalized match
    const normalizedSearchId = this.normalizeId(searchId);
    for (const [id, n] of accountMap.entries()) {
      if (this.normalizeId(id) === normalizedSearchId) {
        return n;
      }
    }
    
    return undefined;
  }

  private buildTree(accounts: ChartOfAccount[]): AccountTreeNode[] {
    if (!accounts || accounts.length === 0) {
      return [];
    }

    const accountMap = new Map<string, AccountTreeNode>();
    const rootNodes: AccountTreeNode[] = [];

    // Step 1: Create nodes for all accounts
    accounts.forEach(account => {
      const node: AccountTreeNode = {
        account,
        children: [],
        level: 0
      };
      accountMap.set(account.accountID, node);
    });

    // Step 2: Build tree structure by linking children to parents
    console.log('\n=== Building Tree Structure ===');
    console.log('Total accounts in map:', accountMap.size);
    console.log('Sample account IDs in map:', Array.from(accountMap.keys()).slice(0, 5));
    
    accounts.forEach(account => {
      const node = accountMap.get(account.accountID);
      if (!node) {
        console.warn(`❌ Node not found for account: ${account.accountID} (${account.accountCode})`);
        return;
      }

      if (account.parentAccountID) {
        // Find parent node using robust ID matching
        const parentNode = this.findNodeById(accountMap, account.parentAccountID);
        
        if (parentNode) {
          // Add this node to its parent's children array
          if (!parentNode.children) {
            parentNode.children = [];
          }
          parentNode.children.push(node);
          node.level = parentNode.level + 1;
          console.log(`✓ Linked: ${account.accountCode} (${account.accountName}) -> Parent: ${parentNode.account.accountCode} (${parentNode.account.accountName})`);
          console.log(`  Child ID: ${account.accountID}, Parent ID: ${account.parentAccountID}, Parent Node ID: ${parentNode.account.accountID}`);
        } else {
          // Parent not found in the list, treat as root
          console.warn(`⚠ Parent not found for account ${account.accountCode} (${account.accountName})`);
          console.warn(`  Looking for parent ID: ${account.parentAccountID} (normalized: ${this.normalizeId(account.parentAccountID)})`);
          console.warn(`  Available account IDs (first 10):`);
          Array.from(accountMap.keys()).slice(0, 10).forEach(id => {
            console.warn(`    - ${id} (normalized: ${this.normalizeId(id)})`);
          });
          rootNodes.push(node);
        }
      } else {
        // Root node (no parent)
        rootNodes.push(node);
        console.log(`✓ Root: ${account.accountCode} (${account.accountName}) - ID: ${account.accountID}`);
      }
    });
    
    // Verify tree structure after building
    console.log('\n=== Tree Structure Verification ===');
    const verifyTreeStructure = (nodes: AccountTreeNode[], level: number = 0): void => {
      nodes.forEach(node => {
        const indent = '  '.repeat(level);
        console.log(`${indent}${node.account.accountCode} - ${node.account.accountName} [${node.children.length} children]`);
        if (node.children.length > 0) {
          verifyTreeStructure(node.children, level + 1);
        }
      });
    };
    verifyTreeStructure(rootNodes);
    console.log('===================================\n');

    // Step 3: Sort children by account code recursively
    const sortChildren = (nodes: AccountTreeNode[]) => {
      nodes.sort((a, b) => a.account.accountCode.localeCompare(b.account.accountCode));
      nodes.forEach(node => {
        if (node.children && node.children.length > 0) {
          sortChildren(node.children);
        }
      });
    };

    sortChildren(rootNodes);
    
    // Debug: Log tree structure
    const logTree = (nodes: AccountTreeNode[], indent: string = ''): void => {
      nodes.forEach(node => {
        console.log(`${indent}${node.account.accountCode} - ${node.account.accountName} (${node.children.length} children)`);
        if (node.children.length > 0) {
          logTree(node.children, indent + '  ');
        }
      });
    };
    
    console.log('=== Tree Structure Built ===');
    console.log(`Total accounts: ${accounts.length}`);
    console.log(`Root nodes: ${rootNodes.length}`);
    logTree(rootNodes);
    console.log('===========================');
    
    return rootNodes;
  }
}
