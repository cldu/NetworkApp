import { Photo } from './photo';

export interface User {
  id: number;
  userName: string;
  nickname: string;
  age: number;
  gender: string;
  created: Date;
  lastActive: Date;
  photoUrl: string;
  city: string;
  country: string;
  interests?: string;
  introduction?: string;
  photos?: Photo[];
  roles?: string[];
}
