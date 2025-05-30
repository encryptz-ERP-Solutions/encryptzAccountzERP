import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SnackbarComponent } from '../components/snackbar/snackbar.component';


@Injectable({
  providedIn: 'root'
})
export class CommonService {

  constructor(
    private snackBar: MatSnackBar
  ) { }

   showSnackbar(message: string, status: string, duration: number): void {
    this.snackBar.openFromComponent(SnackbarComponent, {
      data: { message, status },
      duration: duration,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

}
