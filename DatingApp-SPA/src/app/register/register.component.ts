import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelReister = new EventEmitter();
  model: any = {};
  constructor(private authService: AuthService) { }


  ngOnInit() {
  }

  register() {
    this.authService.register(this.model).subscribe(() => {
      console.log('registeration successful');
    }, error => {
      console.log(error);
    });
  }

  cancel() {
    this.cancelReister.emit(false);
    console.log('cancelled');
  }

}
