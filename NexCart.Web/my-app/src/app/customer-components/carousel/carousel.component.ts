import { Component, OnInit } from '@angular/core';

export interface CarouselSlide {
  id: string;
  image: string;
  fallbackUrl: string;
  title: string;
  description: string;
  alt: string;
}

@Component({
  standalone: false,
  selector: 'app-carousel',
  templateUrl: './carousel.component.html',
  styleUrls: ['./carousel.component.css']
})
export class CarouselComponent implements OnInit {

  slides: CarouselSlide[] = [
    {
      id: 'slide-0',
      image: 'assets/images/carousel/banner-laptop.jpg',
      fallbackUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=1500&h=500&q=80',
      title: 'High Performance Laptops',
      description: 'Power through work and gaming with next-gen speed and brilliant displays.',
      alt: 'High Performance Laptops'
    },
    {
      id: 'slide-1',
      image: 'assets/images/carousel/banner-deals.jpg',
      fallbackUrl: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1500&h=500&q=80',
      title: 'Gaming Gear & Battlestations',
      description: 'Immersive accessories and components built for esports performance.',
      alt: 'Gaming Battlestations and Gear'
    },
    {
      id: 'slide-2',
      image: 'assets/images/carousel/banner-audio.jpg',
      fallbackUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1500&h=500&q=80',
      title: 'Studio Quality Audio',
      description: 'Experience crystal-clear acoustics and active noise cancellation.',
      alt: 'Studio Audio and Headphones'
    },
    {
      id: 'slide-3',
      image: 'assets/images/carousel/banner-mouse.jpg',
      fallbackUrl: 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=1500&h=500&q=80',
      title: 'Precision Gaming Mice & Peripherals',
      description: 'Engineered for responsive ergonomics, sub-millisecond clicks, and speed.',
      alt: 'Precision Gaming Peripherals'
    },
    {
      id: 'slide-4',
      image: 'assets/images/carousel/banner-workspace.jpg',
      fallbackUrl: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=1500&h=500&q=80',
      title: 'Modern Workstation Displays',
      description: 'Expand your vision with ultra-wide screens and ergonomic productivity setups.',
      alt: 'Modern Workspaces and Displays'
    }
  ];

  constructor() { }

  ngOnInit(): void {
  }

  onImageError(event: Event, fallbackUrl: string): void {
    const img = event.target as HTMLImageElement;
    if (img && img.src !== fallbackUrl) {
      img.src = fallbackUrl;
    }
  }

}

