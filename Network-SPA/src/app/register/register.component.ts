import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() registerCanceled = new EventEmitter();
  user: User;
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService, private alertify: AlertifyService, private formBuilder: FormBuilder,
    private router: Router) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-dark-blue',
      maxDate: new Date()
    };
    this.createRegistrationForm();
  }

  createRegistrationForm() {
    this.registerForm = this.formBuilder.group({
      gender: ['male'],
      username: ['', Validators.required],
      nickname: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
      confirmPassword: ['', Validators.required]
    }, { validator: this.passwordMathValidator });
  }

  passwordMathValidator(group: FormGroup) {
    return group.get('password').value === group.get('confirmPassword').value ? null : { 'notSame': true };
  }

  register() {
    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
        this.alertify.success('Registration successful.');
      }, error => this.alertify.error(error),
      () => {
        this.authService.login(this.user).subscribe(() => {
          this.router.navigate(['/members']);
        });
      });
    }
  }

  cancel() {
    this.registerCanceled.emit(false);
  }

}
