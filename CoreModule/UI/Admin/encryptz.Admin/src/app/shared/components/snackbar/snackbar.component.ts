import { Component, Inject, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MAT_SNACK_BAR_DATA, MatSnackBarActions, MatSnackBarLabel, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-snackbar',
  imports: [MatButtonModule, MatIconModule, MatSnackBarActions, MatSnackBarLabel],
  templateUrl: './snackbar.component.html',
  styleUrl: './snackbar.component.scss',
  standalone: true,
})
export class SnackbarComponent {

  snackBarRef = inject(MatSnackBarRef);
  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: any) { }
  
  getSnackStatus(status: string) {
    const statusMap: { [key: string]: string } = {
      'SUCCESS': 'success',
      'WARN': 'warn',
      'INFO': 'info'
    };
    return statusMap[status] || 'error'
  }

  getSnackStatusIcon(status: string) {
    const statusMap: { [key: string]: string } = {
      'SUCCESS': 'check_circle',
      'WARN': 'warning',
      'INFO': 'info'
    };
    return statusMap[status] || 'error'
  }

}
